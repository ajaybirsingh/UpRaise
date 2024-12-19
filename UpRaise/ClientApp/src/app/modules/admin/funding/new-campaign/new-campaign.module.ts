import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';
import { Route, RouterModule } from '@angular/router';
import { FuseCardModule } from '@fuse/components/card';
import { AuthModule } from 'app/core/auth/auth.module';
import { NewCampaignComponent } from 'app/modules/admin/funding/new-campaign/new-campaign.component';
import { SharedModule } from 'app/shared/shared.module';
import { ComponentModule } from '../../../../components/component.module';
import { GoogleMapsResolver } from '../../../landing/home/home.resolvers';
import { NewCampaignResolver } from './new-campaign.resolvers';
import { SaveCampaignComponent } from './save-campaign.component';
import { OrganizationModule } from './steps/organization/organization.module';
import { PeopleModule } from './steps/people/people.module';
import { Step1Component } from './steps/step-1.component';

export const routes: Route[] = [
    {
        path     : '',
        component: NewCampaignComponent,
        resolve: {
            apiLoaded: GoogleMapsResolver,
        }
    },
    {
        path: ':id',
        component: NewCampaignComponent,
        resolve: {
            editCampaign: NewCampaignResolver
        }
    }
];

@NgModule({
    declarations: [
        Step1Component,
        SaveCampaignComponent,
        NewCampaignComponent,
    ],
    imports     : [
        RouterModule.forChild(routes),

        AuthModule,
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatRadioModule,
        MatSelectModule,
        MatStepperModule,
        SharedModule,
        HttpClientModule,
        ComponentModule,
        MatDialogModule,
        OrganizationModule,
        PeopleModule,

        FuseCardModule,
    ],

})
export class NewCampaignModule
{
}
