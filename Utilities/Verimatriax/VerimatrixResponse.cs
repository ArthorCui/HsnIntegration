using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Verimatriax
{
    public class VerimatrixResponse
    {
        public string Response { get; private set; }

        public string ReturnCode
        {
            get
            {
                return Response.Split(VerimatrixConstants.MESSAGE_RETURN_CODE);
            }
        }

        public VerimatrixResponse(string response)
        {
            Response = response;
        }

        public Enums.Status Status
        {
            get
            {
                if (Convert.ToInt32(ReturnCode) == 0)
                {
                    if (Response.IndexOf(VerimatrixConstants.REGISTRATION_OK, 0, StringComparison.CurrentCultureIgnoreCase) > 0)
                    {
                        return Enums.Status.Register;
                    }
                    else if (Response.IndexOf(VerimatrixConstants.SHUTDOWN, 0, StringComparison.CurrentCultureIgnoreCase) > 0)
                    {
                        return Enums.Status.Shutdown;
                    }
                    else
                    {
                        return Enums.Status.Success;
                    }
                }
                else
                {
                    return Enums.Status.Error;
                }
            }
        }
    }
}
