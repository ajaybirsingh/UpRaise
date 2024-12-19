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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { SharedModule } from 'app/shared/shared.module';
import { HttpClientModule } from '@angular/common/http';
import { FuseCardModule } from '@fuse/components/card';
import { MatChipsModule } from '@angular/material/chips';
import { MenuModule } from "headlessui-angular";
import { ComponentModule } from '../../../../../../components/component.module';
import { Step2Component as OrganizationStep2Component } from './step-2.component';
import { Step3Component as OrganizationStep3Component } from './step-3.component';
import { Step4Component as OrganizationStep4Component } from './step-4.component';
import { Step5Component as OrganizationStep5Component } from './step-5.component';
import { Step6Component as OrganizationStep6Component } from './step-6.component';
import { Step7Component as OrganizationStep7Component } from './step-7.component';
import { Step8Component as OrganizationStep8Component } from './step-8.component';
import { NgxFilesizeModule } from 'ngx-filesize';
import { GoogleMapsModule } from '@angular/google-maps';
@NgModule({
    declarations: [
        OrganizationStep2Component,
        OrganizationStep3Component,
        OrganizationStep4Component,
        OrganizationStep5Component,
        OrganizationStep6Component,
        OrganizationStep7Component,
        OrganizationStep8Component,
    ],
    imports: [
        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatRadioModule,
        MatSelectModule,
        MatStepperModule,
        SharedModule,
        MatChipsModule,
        HttpClientModule,
        ComponentModule,
        FuseCardModule,
        MenuModule,
        MatDatepickerModule,
        NgxFilesizeModule,
        GoogleMapsModule
    ],
    exports: [
        OrganizationStep2Component,
        OrganizationStep3Component,
        OrganizationStep4Component,
        OrganizationStep5Component,
        OrganizationStep6Component,
        OrganizationStep7Component,
        OrganizationStep8Component,
    ]
})
export class OrganizationModule
{
    public static maxStep:number = 8;
}
