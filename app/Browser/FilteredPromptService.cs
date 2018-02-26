using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Gecko;

namespace app
{

    internal class FilteredPromptService : nsIPromptService2, nsIPrompt
    {
        private static PromptService _promptService = new PromptService();

        public void Alert([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string dialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string text)
        {
        }

        public void Alert(nsIDOMWindow aParent, string aDialogTitle, string aText)
        {
            ////if (/*want default behaviour */)
            ////{
            ////    _promptService.Alert(aDialogTitle, aText);
            ////}
            // Else do nothing 
        }

        public void AlertCheck([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string dialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string text, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string checkMsg, [MarshalAs(UnmanagedType.U1)] ref bool checkValue)
        {
        }

        public void AlertCheck([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aDialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aText, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aCheckMsg, [MarshalAs(UnmanagedType.U1)] ref bool aCheckState)
        {
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        public nsICancelable AsyncPromptAuth([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.Interface)] nsIChannel aChannel, [MarshalAs(UnmanagedType.Interface)] nsIAuthPromptCallback aCallback, [MarshalAs(UnmanagedType.Interface)] nsISupports aContext, uint level, [MarshalAs(UnmanagedType.Interface)] nsIAuthInformation authInfo, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string checkboxLabel, [MarshalAs(UnmanagedType.U1)] ref bool checkValue)
        {
            return null;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool Confirm([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string dialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string text)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool Confirm([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aDialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aText)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool ConfirmCheck([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string dialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string text, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string checkMsg, [MarshalAs(UnmanagedType.U1)] ref bool checkValue)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool ConfirmCheck([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aDialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aText, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aCheckMsg, [MarshalAs(UnmanagedType.U1)] ref bool aCheckState)
        {
            return true;
        }

        public int ConfirmEx([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string dialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string text, uint buttonFlags, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string button0Title, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string button1Title, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string button2Title, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string checkMsg, [MarshalAs(UnmanagedType.U1)] ref bool checkValue)
        {
            return 1;
        }

        public int ConfirmEx([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aDialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aText, uint aButtonFlags, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aButton0Title, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aButton1Title, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aButton2Title, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aCheckMsg, [MarshalAs(UnmanagedType.U1)] ref bool aCheckState)
        {
            return 1;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool Prompt([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string dialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string text, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] ref string value, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string checkMsg, [MarshalAs(UnmanagedType.U1)] ref bool checkValue)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool Prompt([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aDialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aText, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] ref string aValue, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aCheckMsg, [MarshalAs(UnmanagedType.U1)] ref bool aCheckState)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool PromptAuth([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.Interface)] nsIChannel aChannel, uint level, [MarshalAs(UnmanagedType.Interface)] nsIAuthInformation authInfo, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string checkboxLabel, [MarshalAs(UnmanagedType.U1)] ref bool checkValue)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool PromptPassword([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string dialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string text, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] ref string password, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string checkMsg, [MarshalAs(UnmanagedType.U1)] ref bool checkValue)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool PromptPassword([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aDialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aText, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] ref string aPassword, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aCheckMsg, [MarshalAs(UnmanagedType.U1)] ref bool aCheckState)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool PromptUsernameAndPassword([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string dialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string text, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] ref string username, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] ref string password, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string checkMsg, [MarshalAs(UnmanagedType.U1)] ref bool checkValue)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool PromptUsernameAndPassword([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aDialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aText, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] ref string aUsername, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] ref string aPassword, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aCheckMsg, [MarshalAs(UnmanagedType.U1)] ref bool aCheckState)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool Select([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string dialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string text, uint count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] IntPtr[] selectList, ref int outSelection)
        {
            return true;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public bool Select([MarshalAs(UnmanagedType.Interface)] nsIDOMWindow aParent, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aDialogTitle, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.WStringMarshaler")] string aText, uint aCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] IntPtr[] aSelectList, ref int aOutSelection)
        {
            return true;
        }

        // TODO: implement other methods in similar fashion. (returning appropriate return values)
    }
}
