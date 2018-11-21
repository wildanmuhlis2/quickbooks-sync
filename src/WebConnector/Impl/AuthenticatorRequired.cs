﻿using QbSync.WebConnector.Core;
using System.Threading.Tasks;

namespace QbSync.WebConnector.Impl
{
    public class AuthenticatorRequired : IAuthenticator
    {
        public AuthenticatorRequired()
        {
            throw new System.Exception("You must register an IAuthenticator in order to use the WebConnector.");
        }

        public Task<IAuthenticatedTicket> GetAuthenticationFromLoginAsync(string login, string password)
        {
            throw new System.NotImplementedException();
        }

        public Task<IAuthenticatedTicket> GetAuthenticationFromTicketAsync(string ticket)
        {
            throw new System.NotImplementedException();
        }

        public Task SaveTicketAsync(IAuthenticatedTicket ticket)
        {
            throw new System.NotImplementedException();
        }
    }
}
