using System.Globalization;

namespace GreenTrade.Client.Services;

/// <summary>
/// Service to handle formatting for dates and currency following Brazilian standards (pt-BR).
/// </summary>
public class FormatterService
{
    private readonly CultureInfo _culture = new CultureInfo("pt-BR");

    public string FormatCurrency(decimal value)
    {
        return value.ToString("C", _culture);
    }

    public string FormatDate(DateTime date)
    {
        return date.ToString("d", _culture);
    }

    public string FormatDateTime(DateTime date)
    {
        return date.ToString("g", _culture);
    }
}
