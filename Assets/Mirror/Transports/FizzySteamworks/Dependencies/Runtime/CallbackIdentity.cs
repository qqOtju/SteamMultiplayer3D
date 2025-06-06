// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2019 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

using System;
#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

namespace Steamworks
{
    internal class CallbackIdentities
    {
        public static int GetCallbackIdentity(Type callbackStruct)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX
            foreach (CallbackIdentityAttribute attribute in callbackStruct.GetCustomAttributes(
                         typeof(CallbackIdentityAttribute), false)) return attribute.Identity;
#endif
            throw new Exception("Callback number not found for struct " + callbackStruct);
        }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    internal class CallbackIdentityAttribute : Attribute
    {
        public CallbackIdentityAttribute(int callbackNum)
        {
            Identity = callbackNum;
        }

        public int Identity { get; set; }
    }
}

#endif // !DISABLESTEAMWORKS