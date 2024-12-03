using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerWebPageBlazorWebAppShared.Policies;
public static  class Policies
{
  public enum TokenTypePolicyValues
  {
    UnknownPolicy,    //always first to have a good default value
    WebChatTokenPolicy,
    DealershipChatTokenPolicy,
  }
}
