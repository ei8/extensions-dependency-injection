using Nancy.TinyIoc;
using neurUL.Common.Http;
using System.Net.Http;

namespace ei8.Extensions.DependencyInjection
{
    public static class TinyIoCContainerExtensions
    {
        public static void AddRequestProvider(this TinyIoCContainer container)
        {
            var rp = new RequestProvider();
            rp.SetHttpClientHandler(new HttpClientHandler());
            container.Register<IRequestProvider>(rp);
        }
    }
}
