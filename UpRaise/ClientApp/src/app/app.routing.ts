import { Route, UrlMatchResult, UrlSegment } from '@angular/router';
import { AuthGuard } from 'app/core/auth/guards/auth.guard';
import { NoAuthGuard } from 'app/core/auth/guards/noAuth.guard';
import { LayoutComponent } from 'app/layout/layout.component';
import { InitialDataResolver } from 'app/app.resolvers';
import { isCampaign } from './modules/landing/home/home.matchers';
import { PrivacyComponent } from './modules/pages/privacy/privacy.component';
import { TermsOfServiceComponent } from './modules/pages/terms-of-service/terms-of-service.component';




// @formatter:off
// tslint:disable:max-line-length
export const appRoutes: Route[] = [

    // Redirect empty path to '/example'
    { path: '', pathMatch: 'full', redirectTo: 'home' },

    // Redirect signed in user to the '/example'
    //
    // After the user signs in, the sign in page will redirect the user to the 'signed-in-redirect'
    // path. Below is another redirection for that path to redirect the user to the desired
    // location. This is a small convenience to keep all main routes together here on this file.
    { path: 'signed-in-redirect', pathMatch: 'full', redirectTo: 'dashboards/campaign' },

    { path: 'terms-of-service', canActivate: [NoAuthGuard], component: TermsOfServiceComponent },
    { path: 'privacy', canActivate: [NoAuthGuard], component: PrivacyComponent },

    // Auth routes for guests
    {
        path: '',
        canActivate: [NoAuthGuard],
        canActivateChild: [NoAuthGuard],
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            { path: 'confirmation-required', loadChildren: () => import('app/modules/auth/confirmation-required/confirmation-required.module').then(m => m.AuthConfirmationRequiredModule) },
            { path: 'forgot-password', loadChildren: () => import('app/modules/auth/forgot-password/forgot-password.module').then(m => m.AuthForgotPasswordModule) },
            { path: 'reset-password', loadChildren: () => import('app/modules/auth/reset-password/reset-password.module').then(m => m.AuthResetPasswordModule) },
            { path: 'sign-in', loadChildren: () => import('app/modules/auth/sign-in/sign-in.module').then(m => m.AuthSignInModule) },
            { path: 'sign-up', loadChildren: () => import('app/modules/auth/sign-up/sign-up.module').then(m => m.AuthSignUpModule) }
        ]
    },

    // Auth routes for authenticated users
    {
        path: '',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            { path: 'sign-out', loadChildren: () => import('app/modules/auth/sign-out/sign-out.module').then(m => m.AuthSignOutModule) },
            { path: 'unlock-session', loadChildren: () => import('app/modules/auth/unlock-session/unlock-session.module').then(m => m.AuthUnlockSessionModule) }
        ]
    },

    // Landing routes
    {
        path: '',
        component: LayoutComponent,
        data: {
            layout: 'empty'
        },
        children: [
            { path: 'home', loadChildren: () => import('app/modules/landing/home/home.module').then(m => m.LandingHomeModule) },
            { matcher: isCampaign, loadChildren: () => import('app/modules/landing/home/home.module').then(m => m.LandingHomeSEOModule) },
        ]
    },

    //{ matcher: matchCampaigns, component: LayoutComponent },

    // Admin routes
    {
        path: '',
        canActivate: [AuthGuard],
        canActivateChild: [AuthGuard],
        component: LayoutComponent,
        resolve: {
            initialData: InitialDataResolver,
        },
        children: [
            // Dashboards
            {
                path: 'dashboards', children: [
                    { path: 'campaign', loadChildren: () => import('app/modules/admin/dashboards/campaign/campaign.module').then(m => m.CampaignModule) },
                ]
            },

            // Funding
            {
                path: 'funding', children: [
                    //{ path: 'campaign', loadChildren: () => import('app/modules/admin/dashboards/project/project.module').then(m => m.ProjectModule) },
                    { path: 'new-campaign', loadChildren: () => import('app/modules/admin/funding/new-campaign/new-campaign.module').then(m => m.NewCampaignModule) },
                    { path: 'your-campaigns', loadChildren: () => import('app/modules/admin/funding/your-campaigns/your-campaigns.module').then(m => m.YourCampaignsModule) },
                ]
            },

            // User
            {
                path: 'user',
                children: [

                    { path: 'profile', loadChildren: () => import('app/modules/admin/user/profile/profile.module').then(m => m.ProfileModule) }

                    /*
                    { path: 'messages', loadChildren: () => import('app/modules/admin/dashboards/project/project.module').then(m => m.ProjectModule) },
                    { path: 'system-notifications', loadChildren: () => import('app/modules/admin/dashboards/project/project.module').then(m => m.ProjectModule) },
                    */
                    //{ path: 'settings', loadChildren: () => import('app/modules/admin/user/settings/project.module').then(m => m.ProjectModule) },
                ]
            },


            // System
            {
                path: 'help-center', loadChildren: () => import('app/modules/admin/help-center/help-center.module').then(m => m.HelpCenterModule),
            },

            {
                path: 'campaign-redline', loadChildren: () => import('app/modules/admin/campaign-redlining/campaign-redlining.module').then(m => m.CampaignRedliningModule)
            },

        ]
    },

    // 404 & Catch all
    { path: '404-not-found', pathMatch: 'full', loadChildren: () => import('app/modules/pages/error/error-404/error-404.module').then(m => m.Error404Module) },

    //{ path: '**', pathMatch: 'full', redirectTo: '404-not-found' },
    { path: '**', pathMatch: 'full', redirectTo: '/home' },

];

