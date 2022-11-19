﻿using CrestApps.RetsSdk.Exceptions;
using CrestApps.RetsSdk.Models.Enums;
using System;
using System.Collections.Generic;

namespace CrestApps.RetsSdk.Models
{
    public class SessionResource
    {
        public string SessionId { get; set; }
        public string Cookie { get; set; }

        public Dictionary<Capability, Uri> Capabilities { get; set; }

        public SessionResource()
        {
            Capabilities = new Dictionary<Capability, Uri>();
        }

        public void AddCapability(Capability name, string url)
        {
            var uri = new Uri(url);

            if (Capabilities.ContainsKey(name) || !uri.IsWellFormedOriginalString())
            {
                return;
            }

            if (!Capabilities.ContainsKey(name))
            {
                Capabilities.Add(name, uri);
            }
        }

        public Uri GetCapability(Capability name)
        {
            if(!Capabilities.ContainsKey(name))
            {
                throw new MissingCapabilityException();
            }

            return Capabilities[name];
        }

    }
}
