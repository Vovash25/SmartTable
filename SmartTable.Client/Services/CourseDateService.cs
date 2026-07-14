@using SmartTable.Client.Models
@inject SmartTable.Client.Services.LocalizationService LocalizationSvc

<MudDialog>
    <DialogContent>
        <MudForm @ref="_form" @bind-IsValid="_isValid">
            <MudStack Spacing="3">
                <MudDatePicker @bind-Date="_startDate"
                               Label="@LocalizationSvc.GetString("StartDateLabel")"
                               Variant="Variant.Outlined"
                               DateFormat="dd.MM.yyyy"
                               Required="true"
                               RequiredError="@LocalizationSvc.GetString("RequiredField")" />

                <MudDatePicker @bind-Date="_endDate"
                               Label="@LocalizationSvc.GetString("EndDateLabelShort")"
                               Variant="Variant.Outlined"
                               DateFormat="dd.MM.yyyy" />
            </MudStack>
        </MudForm>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel" Color="Color.Default" Variant="Variant.Text">
            @LocalizationSvc.GetString("Cancel")
        </MudButton>
        <MudButton OnClick="Submit" Color="Color.Primary" Variant="Variant.Filled" Disabled="!_isValid || _startDate is null">
            @LocalizationSvc.GetString("Add")
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public Guid CourseId { get; set; }

    private MudForm _form = null!;
    private bool _isValid;
    private DateTime? _startDate = DateTime.Today;
    private DateTime? _endDate;

    private async Task Submit()
    {
        await _form.ValidateAsync();
        if (!_isValid || _startDate is null) return;

        var model = new CourseDate
        {
            CourseId = CourseId,
            StartDate = _startDate.Value,
            EndDate = _endDate
        };

        MudDialog.Close(DialogResult.Ok(model));
    }

    private void Cancel() => MudDialog.Cancel();
}
