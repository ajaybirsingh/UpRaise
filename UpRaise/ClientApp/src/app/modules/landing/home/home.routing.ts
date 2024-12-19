import { Route } from '@angular/router';
import { LandingHomeComponent } from 'app/modules/landing/home/home.component';
import { ContributionComponent } from './contribution/contribution.component';
import { LandingHomeDetailsComponent } from './details/details.component';
import { ClosestLocationResolver, GoogleMapsResolver, LandingHomeCampaignResolver, LandingHomeCampaignResolverSEOFriendlyURL, LandingHomeCategoriesResolver, LandingHomeListIsSignedInResolver } from './home.resolvers';
import { ExploreComponent } from './explore/explore.component';
import { AuthGuard } from '../../../core/auth/guards/auth.guard';


export const landingHomeRoutes: Route[] = [

    {
        path: '',
        component: LandingHomeComponent,
        resolve: {
            categories: LandingHomeCategoriesResolver
        },
        children: [
            {
                path: '',
                pathMatch: 'full',
                component: ExploreComponent,
                resolve: {
                    isSignedIn: LandingHomeListIsSignedInResolver,
                    apiLoaded: GoogleMapsResolver,
                    closestLocation: ClosestLocationResolver
                }
            },

            {
                path: 'detail/:type/:id',
                component: LandingHomeDetailsComponent,
                resolve: {
                    campaign: LandingHomeCampaignResolver,
                    isSignedIn: LandingHomeListIsSignedInResolver
                }
            },

            {
                path: 'contribution/:type/:id',
                canActivate: [AuthGuard],
                canActivateChild: [AuthGuard],
                component: ContributionComponent,
                resolve: {
                    campaign: LandingHomeCampaignResolver,
                    isSignedIn: LandingHomeListIsSignedInResolver
                }
            }



        ]
    }
];


export const landingHomeRoutesSEO: Route[] = [
    {
        path: '',
        pathMatch: 'full',
        component: LandingHomeDetailsComponent,
        resolve: {
            campaign: LandingHomeCampaignResolverSEOFriendlyURL,
            isSignedIn: LandingHomeListIsSignedInResolver
        }
    },
    {
        path: 'contribution',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: ContributionComponent,
        resolve: {
            campaign: LandingHomeCampaignResolverSEOFriendlyURL,
            isSignedIn: LandingHomeListIsSignedInResolver
        }
    }

];

