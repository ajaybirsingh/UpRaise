import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { SharedModule } from 'app/shared/shared.module';
import { LandingHomeComponent } from 'app/modules/landing/home/home.component';
import { landingHomeRoutes, landingHomeRoutesSEO } from 'app/modules/landing/home/home.routing';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FuseFindByKeyPipeModule } from '@fuse/pipes/find-by-key';
import { MatTabsModule } from '@angular/material/tabs';
import { LandingHomeDetailsComponent } from './details/details.component';
import { ExploreComponent } from './explore/explore.component';
import { ExploreListComponent } from './explore/explore-list.component';
import { ExploreMapComponent } from './explore/explore-map.component';

import { FuseCardModule } from '../../../../@fuse/components/card';
import { SharedPipesModule } from '../../../core/pipes/SharedPipesModule';
import { ContactDialog } from 'app/components/contact/contact.dialog';
import { ShareDialog } from 'app/components/share/share.dialog';
import { MatDialogModule } from '@angular/material/dialog';
import { MatStepperModule } from '@angular/material/stepper';
import { MatDividerModule } from '@angular/material/divider';

import { ContributionsComponent } from './details/contributions.component';
import { ContributionComponent } from './contribution/contribution.component';

import { CryptoComponent } from './contribution/types/crypto.component';
import { GPayComponent } from './contribution/types/gpay.component';
import { StripeComponent } from './contribution/types/stripe.component';
import { ETransferComponent } from './contribution/types/etransfer.component';
import { UpraiseFooterComponent } from '../../../components/upraisefooter/upraisefooter.component';
import { UpraiseHeaderComponent } from '../../../components/upraiseheader/upraiseheader.component';
import { GoogleMapsModule } from '@angular/google-maps';
import { HttpClientModule, HttpClientJsonpModule } from '@angular/common/http';

@NgModule({
    declarations: [
        LandingHomeComponent,
        LandingHomeDetailsComponent,

        ExploreComponent,
        ExploreListComponent,
        ExploreMapComponent,

        ContributionsComponent,
        ContributionComponent,
        ContactDialog,
        ShareDialog,

        CryptoComponent,
        GPayComponent,
        StripeComponent,
        ETransferComponent,
        UpraiseFooterComponent,
        UpraiseHeaderComponent
    ],
    exports     : [
        LandingHomeComponent,
        LandingHomeDetailsComponent,

        ExploreComponent,
        ExploreListComponent,
        ExploreMapComponent,

        ContributionsComponent,
        ContributionComponent,
        ContactDialog,
        ShareDialog,

        CryptoComponent,
        GPayComponent,
        StripeComponent,
        ETransferComponent,
        UpraiseFooterComponent,
        UpraiseHeaderComponent
    ],
    imports: [
        HttpClientModule,
        HttpClientJsonpModule,
        GoogleMapsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatDividerModule,
        MatProgressBarModule,
        MatStepperModule,
        MatSelectModule,
        MatSidenavModule,
        MatSlideToggleModule,
        MatTooltipModule,
        FuseFindByKeyPipeModule,
        FuseCardModule,
        SharedModule,
        MatTabsModule,
        SharedPipesModule,
        MatDialogModule,
        RouterModule,
    ]
})
export class LandingHomeCommonModule
{
}
