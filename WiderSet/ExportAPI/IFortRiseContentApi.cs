namespace FortRise.Content;

public partial interface IFortRiseContentApi 
{
    ILoaderAPI LoaderApi { get; }
    IThemeAPI Themes { get; }
    IVersusLoaderAPI VersusLoaders { get; }
}
