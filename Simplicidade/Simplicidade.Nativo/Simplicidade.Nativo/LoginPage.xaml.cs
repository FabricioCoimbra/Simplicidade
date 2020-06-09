using Simplicidade.Compartilhada.Model;
using Simplicidade.Compartilhada.Model.ViewModel;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Simplicidade.Nativo
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public UsuarioViewModel UsuarioViewModel { get; set; }

        public LoginPage()
        {
            InitializeComponent();
        }

        async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var usuario = new Usuario()
            {
                Username = UsuarioEntry.Text,
                Password = SenhaEntry.Text
            };

            var isValid = await ValidaCredenciais(usuario);
            if (isValid)
            {
                App.IsUserLoggedIn = true;
                Navigation.InsertPageBefore(new MainPage(), this);
                await Navigation.PopAsync();
            }
            else
            {
                AvisoErroSucesso.Text = "Login falhou";
                SenhaEntry.Text = string.Empty;
            }
        }

        //TODO criar os serviços de comunicação HTTP padrão para a aplicação ...
        private async Task<bool> ValidaCredenciais(Usuario usuario)
        {
            using (var client = new HttpClient(GetInsecureHandler()))
            {
                //TODO implementar as devidas tratativas para falhas, tanto de conexão quanto erros retornados do servidor etc...
                // acho q precisa um serviço de comunicação com o servidor...
                client.BaseAddress = new Uri(Device.RuntimePlatform
                                             == Device.Android ? "https://10.0.2.2:5001" : "https://localhost:5001");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync("api/Usuarios/Login", usuario);
                    //httpResponseMessage.EnsureSuccessStatusCode();

                    UsuarioViewModel = await httpResponseMessage.Content.ReadAsAsync<UsuarioViewModel>();

                    //TODO Esconder essa informação.
                    await DisplayAlert("Aviso", UsuarioViewModel.Token, "OK");

                    return httpResponseMessage.IsSuccessStatusCode;
                }
                catch (HttpRequestException e)
                {
                    await DisplayAlert("Erro", e.Message, "OK");
                }
            }
            return false;
        }

        private HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            return handler;
        }
    }
}