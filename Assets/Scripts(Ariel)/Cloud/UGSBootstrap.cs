using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class UGSBootstrap : MonoBehaviour
{
    async void Awake()
    {
        await InitUGS();
    }

    private static async Task InitUGS()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
            return;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("UGS: signed in as " + AuthenticationService.Instance.PlayerId);
        }
    }
}
