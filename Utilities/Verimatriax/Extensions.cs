using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Verimatriax
{
    public static class Extensions
    {
        public static string Split(this string message, int index)
        {
            if (!string.IsNullOrEmpty(message))
            {
                try
                {
                    return message.Split(VerimatrixConstants.COMMA)[index];
                }
                catch (Exception)
                {
                    //ex
                }
            }
            return string.Empty;
        }

        public static string GetMessageNumber(this string message)
        {
            return message.Split(VerimatrixConstants.MESSAGE_NUMBER_INDEX);
        }

        public static string GetCommandName(this string message)
        {
            return message.Split(VerimatrixConstants.MESSAGE_COMMAND_NAME_INDEX);
        }
    }
}
