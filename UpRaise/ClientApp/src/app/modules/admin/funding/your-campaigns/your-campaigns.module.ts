import { NgModule } from '@angular/core';
import { Route, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';
import { SharedModule } from 'app/shared/shared.module';
import { YourCampaignsComponent } from './your-campaigns.component';
import { HttpClientModule } from '@angular/common/http';

import { FuseCardModule } from '@fuse/components/card';
import { AuthModule } from 'app/core/auth/auth.module';

import { ComponentModule } from '../../../../components/component.module';


export const routes: Route[] = [
    {
        path     : '',
        component: YourCampaignsComponent
    }
];

@NgModule({
    declarations: [
        YourCampaignsComponent,
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
        FuseCardModule,
    ],

})
export class YourCampaignsModule
{
}
