using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DataAccess;
using DataAccess.Helpers;
using DataAccess.OAuth;
using DataAccess.Responses;
using DataAccess.Responses.Impl;
using DataAccess.Sources;
using Logic;
using Logic.Handlers;
using UI.ViewModels;

namespace UI
{
    public partial class App : Application
    {
        public IServiceProvider Container { get; private set; }
        public ISettingsManager Settings { private get; set; } = SettingsAccess.GetInstance();

        public App()
        {
            RegisterServices();
        }

        private void RegisterServices()
        {
            var services = new InversionContainer();

            //Specialized HttpClients
            var clientImgur = new HttpClient();
            var imgurId = string.IsNullOrWhiteSpace(Settings.GetImgurClientID())
                ? Settings.GetBuiltinImgurClientID()
                : Settings.GetImgurClientID();

            clientImgur.DefaultRequestHeaders.Add("Authorization", $"Client-ID {imgurId}");

            var clientReddit = new HttpClient();
            clientReddit.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Settings.GetRedditUserAgent());

            //TODO: Potentially implement oauth-authentication for usage of the deviant-art api https://www.deviantart.com/developers/authentication so we can add a deviantart-tab for downloading images from a user-gallery
            var clientDeviantart = new HttpClient();
            clientDeviantart.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Settings.GetDeviantartUserAgent());

            //Sources
            services.AddTransient<ISource<ImgurRatelimitResponse>>(o => new ImgurRatelimitSource(clientImgur));
            services.AddTransient<ISource<ImgurAlbum>>(o => new ImgurAlbumSource(clientImgur, o.GetService<ImgurRatelimiter>()));
            services.AddTransient<ISource<ImgurImage>>(o => new ImgurImageSource(clientImgur, o.GetService<ImgurRatelimiter>()));
            services.AddTransient<ISource<GenericAlbum>>(o => new ImgurAccountImagesSource(clientImgur, o.GetService<ImgurRatelimiter>()));
            services.AddTransient<ISource<DeviantartImage>>(o => new DeviantartImageSource(clientDeviantart));
            services.AddTransient<ISource<LocalDirectory>, LocalSource>();
            services.AddTransient<ICollectionSource<RedditListing>>(o =>
               new RedditSource(clientReddit, o.GetService<ISource<ImgurAlbum>>(),
                                o.GetService<ISource<ImgurImage>>(), o.GetService<ISource<DeviantartImage>>(),
                                o.GetService<ITokenAcquirer<RedditToken>>()));

            //Handlers
            services.AddTransient<IHandler<ImgurHandler.ImgurFilter, IApiCollection<IApiImage>>>(o => new ImgurHandler(o.GetService<ISource<ImgurAlbum>>(), o.GetService<ISource<GenericAlbum>>()));
            services.AddTransient<IHandler<RedditHandler.RedditFilter, RedditListing>>(o => new RedditHandler(o.GetService<ICollectionSource<RedditListing>>()));
            services.AddTransient<IHandler<LocalHandler.LocalFilter, LocalDirectory>>(o => new LocalHandler(o.GetService<ISource<LocalDirectory>>()));

            services.AddTransient<ITokenAcquirer<RedditToken>, RedditAcquirer>();

            services.AddSingleton<ImgurRatelimiter>(new ImgurRatelimiter(services.GetService<ISource<ImgurRatelimitResponse>>()));
            services.AddTransient<Ratelimiter>(o => new Ratelimiter(o.GetService<ImgurRatelimiter>()));

            //Viewmodels
            services.AddTransient<ImgurControlViewModel>(o => new ImgurControlViewModel(o.GetService<IHandler<ImgurHandler.ImgurFilter, IApiCollection<IApiImage>>>()));
            services.AddTransient<RedditControlViewModel>(o => new RedditControlViewModel(o.GetService<IHandler<RedditHandler.RedditFilter, RedditListing>>()));
            services.AddTransient<LocalControlViewModel>(o => new LocalControlViewModel(o.GetService<IHandler<LocalHandler.LocalFilter, LocalDirectory>>()));
            services.AddTransient<SettingsControlViewModel>(o => new SettingsControlViewModel(o.GetService<Ratelimiter>()));

            Container = services;
        }
    }
}
