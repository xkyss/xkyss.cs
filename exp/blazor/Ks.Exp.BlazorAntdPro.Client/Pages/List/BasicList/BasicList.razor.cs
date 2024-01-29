using AntDesign;
using Ks.Exp.BlazorAntdPro.Models;
using Ks.Exp.BlazorAntdPro.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ks.Exp.BlazorAntdPro.Pages.List
{
    public partial class BasicList
    {
        private readonly BasicListFormModel _model = new BasicListFormModel();

        private readonly IDictionary<string, ProgressStatus> _pStatus = new Dictionary<string, ProgressStatus>
        {
            {"active", ProgressStatus.Active},
            {"exception", ProgressStatus.Exception},
            {"normal", ProgressStatus.Normal},
            {"success", ProgressStatus.Success}
        };

        private ListItemDataType[] _data = { };

        [Inject] protected IProjectService ProjectService { get; set; }

        private void ShowModal()
        {
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _data = await ProjectService.GetFakeListAsync(5);
        }
    }
}