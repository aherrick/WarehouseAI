using System.Net.Http.Json;

namespace WarehouseAI.UI.Client.Helpers;

public class CustomHttpClient(
    HttpClient httpClient,
    CustomSweetAlertService customSweetAlertService
)
{
    public async Task<T> Get<T>(string url)
    {
        var response = await httpClient.GetAsync(url);

        return await HandleResponse<T>(response);
    }

    public async Task<T> Post<T>(string url, object value = null)
    {
        var response = await httpClient.PostAsJsonAsync(url, value);

        return await HandleResponse<T>(response);
    }

    private async Task<T> HandleResponse<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            await ShowExceptionAlert(response);

            return default;
        }
        else
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }

    private async Task ShowExceptionAlert(HttpResponseMessage response)
    {
        var data = await response.Content.ReadAsStringAsync();

        await customSweetAlertService.Error(data);
    }
}