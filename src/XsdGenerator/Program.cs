﻿using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace QbSync.XsdGenerator
{
    class Program
    {
        private string fileComment = 
            "------------------------------------------------------------------------------\n" +
            " <auto-generated>\n" +
            "     This code was generated by a tool.\n" +
            "\n" +
            "     Changes to this file may cause incorrect behavior and will be lost if\n" +
            "     the code is regenerated.\n" +
            " </auto-generated>\n" +
            "------------------------------------------------------------------------------";

        public Program(params string[] args)
        {
            XmlSchemas xsds = new XmlSchemas();
            var i = 0;
            while (!args[i].StartsWith("/o:") || i >= args.Length)
            {
                xsds.Add(GetSchema(args[i]));
                i++;
            }

            var output = string.Empty;
            if (args[i].StartsWith("/o:"))
            {
                output = args[i].Substring(3);
            }

            xsds.Compile(null, true);
            XmlSchemaImporter schemaImporter = new XmlSchemaImporter(xsds);

            // create the codedom
            var codeNamespace = new CodeNamespace("QbSync.QbXml.Objects");
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Linq"));
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(codeNamespace);
            var codeExporter = new XmlCodeExporter(codeNamespace, compileUnit, CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateOrder);

            foreach (XmlSchema xsd in xsds)
            {
                ArrayList maps = new ArrayList();
                foreach (XmlSchemaType schemaType in xsd.SchemaTypes.Values)
                {
                    maps.Add(schemaImporter.ImportSchemaType(schemaType.QualifiedName));
                }
                foreach (XmlSchemaElement schemaElement in xsd.Elements.Values)
                {
                    maps.Add(schemaImporter.ImportTypeMapping(schemaElement.QualifiedName));
                }
                foreach (XmlTypeMapping map in maps)
                {
                    codeExporter.ExportTypeMapping(map);
                }
            }

            var typeEnhancer = new TypeEnhancer(codeNamespace, xsds);
            typeEnhancer.Enhance();

            // Add a comment at the top of the file
            var x = fileComment.Split('\n').Select(m => new CodeCommentStatement(m)).ToArray();
            codeNamespace.Comments.AddRange(x);

            // Check for invalid characters in identifiers
            CodeGenerator.ValidateIdentifiers(codeNamespace);

            // output the C# code
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();

            var options = new CodeGeneratorOptions
            {
                BlankLinesBetweenMembers = true,
                BracingStyle = "C",
                ElseOnClosing = true,
                IndentString = "    "
            };

            using (StringWriter writer = new StringWriter())
            {
                codeProvider.GenerateCodeFromCompileUnit(
                    new CodeSnippetCompileUnit("#pragma warning disable 1591"),
                    writer, options);
                codeProvider.GenerateCodeFromNamespace(codeNamespace, writer, options);
                codeProvider.GenerateCodeFromCompileUnit(
                    new CodeSnippetCompileUnit("#pragma warning restore 1591"),
                    writer, options);

                string content = writer.GetStringBuilder().ToString();
                if (string.IsNullOrEmpty(output))
                {
                    Console.WriteLine(content);
                    Console.ReadLine();
                }
                else
                {
                    File.WriteAllText(output, content);
                }
            }
        }

        private XmlSchema GetSchema(string filename)
        {
            XmlSchema xsd;
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                xsd = XmlSchema.Read(stream, null);
            }

            return xsd;
        }

        static void Main(string[] args)
        {
            new Program(args);
        }

        // Remove all the attributes from each type in the CodeNamespace, except
        // System.Xml.Serialization.XmlTypeAttribute
        private void RemoveAttributes(CodeNamespace codeNamespace)
        {
            foreach (CodeTypeDeclaration codeType in codeNamespace.Types)
            {
                CodeAttributeDeclaration xmlTypeAttribute = null;
                foreach (CodeAttributeDeclaration codeAttribute in codeType.CustomAttributes)
                {
                    Console.WriteLine(codeAttribute.Name);
                    if (codeAttribute.Name == "System.Xml.Serialization.XmlTypeAttribute")
                    {
                        xmlTypeAttribute = codeAttribute;
                    }
                }
                codeType.CustomAttributes.Clear();
                if (xmlTypeAttribute != null)
                {
                    codeType.CustomAttributes.Add(xmlTypeAttribute);
                }
            }
        }
    }
}
