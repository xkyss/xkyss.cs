using Ks.Exp.BlazorAntdPro.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Ks.Exp.BlazorAntdPro.Pages.Account.Center
{
    public partial class Articles
    {
        [Parameter] public IList<ListItemDataType> List { get; set; }
    }
}