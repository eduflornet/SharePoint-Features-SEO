using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Contoso.Website.SEO.Properties;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Publishing.Navigation;
using Microsoft.SharePoint.WebPartPages;


namespace SPFeaturesSEO.Features.Props
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("e2c7ff1f-30e7-4775-8dad-8ceadd04ad92")]
    public class Feature1EventReceiver : SPFeatureReceiver
    {
        // 4.	En el evento, en la activación de la feature, activar las etiquetas SEO.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPSite site = (SPSite)properties.Feature.Parent;

            using (SPWeb web = site.RootWeb)
            {
                // configure SEO properties for the home page (does not use Navigation Term SEO properites)
                SPListItem welcomePage = web.GetListItem(web.RootFolder.WelcomePage);
                // Aqui hay 3 Resources descritos en Resources.resx
                welcomePage["SeoBrowserTitle"] = Resources.SeoBrowserTitle;
                welcomePage["SeoMetaDescription"] = Resources.SeoDescription;
                welcomePage["SeoKeywords"] = Resources.SeoKeywords;
                welcomePage["SeoRobotsNoIndex"] = false.ToString();
                welcomePage.SystemUpdate();

                // configure SEO propertie on all navigation terms associated with Welcome Pages

                TaxonomySession taxSession = new TaxonomySession(site, updateCache: true);
                TermStore termStore = taxSession.DefaultSiteCollectionTermStore;
                Group termGroup = termStore.GetSiteCollectionGroup(site, true);

                // locate the navigation term set for the site collection (there can be only one)
                foreach (TermSet termSet in termGroup.TermSets)
                {
                    NavigationTermSet navTermSet = NavigationTermSet.GetAsResolvedByWeb(termSet, site.RootWeb, StandardNavigationProviderNames.GlobalNavigationTaxonomyProvider);
                    if (navTermSet.IsNavigationTermSet)
                    {
                        // determine which navigation nodes are associated with Welcome Page content types
                        foreach (NavigationTerm navTerm in navTermSet.Terms)
                        {
                            string pageUrl = SPUtility.GetServerRelativeUrlFromPrefixedUrl(navTerm.TargetUrl.Value);
                            SPListItem pageItem = web.GetListItem(pageUrl);
                            if (pageItem.ContentType.Name == "Welcome Page")
                            {
                                // set the SEO properties on the Navigation Term (all will have same SEO tags)
                                Term term = termSet.GetTerm(navTerm.Id);
                                term.SetLocalCustomProperty("_Sys_Seo_PropBrowserTitle", Resources.SeoBrowserTitle);
                                term.SetLocalCustomProperty("_Sys_Seo_PropDescription", Resources.SeoDescription);
                                term.SetLocalCustomProperty("_Sys_Seo_PropKeywords", Resources.SeoKeywords);
                                term.SetLocalCustomProperty("_Sys_Seo_PropSiteNoIndex", false.ToString());
                            }
                        }

                        break;
                    }
                }

                termStore.CommitAll();
                web.Update();
            }
        }


    }


    // Uncomment the method below to handle the event raised before a feature is deactivated.

    //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
    //{
    //}


    // Uncomment the method below to handle the event raised after a feature has been installed.

    //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
    //{
    //}


    // Uncomment the method below to handle the event raised before a feature is uninstalled.

    //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
    //{
    //}

    // Uncomment the method below to handle the event raised when a feature is upgrading.

    //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
    //{
    //}

}
