using Microsoft.AspNetCore.Components;

namespace Ks.Exp.BlazorAntdPro.Pages.Dashboard.Monitor
{
    public partial class Pie
    {
        [Parameter]
        public bool? Animate { get; set; }

        [Parameter]
        public int? LineWidth { get; set; }
    }
}