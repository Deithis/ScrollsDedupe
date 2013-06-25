using System;

using ScrollsModLoader.Interfaces;
using UnityEngine;
using Mono.Cecil;
using System.Collections.Generic;

namespace Dedupe
{
	public class Main : BaseMod
	{
        IgnoreDuplicate _ignoreDup = null;

		//initialize everything here, Game is loaded at this point
		public Main ()
		{
            _ignoreDup = new IgnoreDuplicate();
		}


		public static string GetName ()
		{
			return "ChatDedupe";
		}

		public static int GetVersion ()
		{
			return 1;
		}

		//only return MethodDefinitions you obtained through the scrollsTypes object
		//safety first! surround with try/catch and return an empty array in case it fails
		public static MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
            try
            {
                return new MethodDefinition[] {
					scrollsTypes["ChatRooms"].Methods.GetMethod("ChatMessage", new Type[]{typeof(RoomChatMessageMessage)})
				};
            }
            catch
            {
                return new MethodDefinition[] { };
            }
		}

		public override bool BeforeInvoke (InvocationInfo info, out object returnValue)
		{
            returnValue = null;

            if (info.targetMethod.Equals("ChatMessage")) // ChatMessage (received) in ChatRooms
            {
                RoomChatMessageMessage rcmm = (RoomChatMessageMessage)info.arguments[0];
                //return false;
                return _ignoreDup.hooksReceive(rcmm);
            }

			return false;
		}

		public override void AfterInvoke (InvocationInfo info, ref object returnValue)
		{
			return;
		}
	}
}

