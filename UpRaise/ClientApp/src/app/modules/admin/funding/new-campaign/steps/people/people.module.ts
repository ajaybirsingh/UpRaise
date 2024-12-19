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
import { HttpClientModule } from '@angular/common/http';
import { FuseCardModule } from '@fuse/components/card';
import { MatChipsModule } from '@angular/material/chips';
import { MenuModule } from "headlessui-angular";
import { ComponentModule } from '../../../../../../components/component.module';

import { Step2Component as PeopleStep2Component } from './step-2.component';
import { Step3Component as PeopleStep3Component } from './step-3.component';
import { Step4Component as PeopleStep4Component } from './step-4.component';
import { Step5Component as PeopleStep5Component } from './step-5.component';
import { Step6Component as PeopleStep6Component } from './step-6.component';
import { NgxFilesizeModule } from 'ngx-filesize';
import { GoogleMapsModule } from '@angular/google-maps';
import { GeocodeDialog } from 'app/components/geocode/geocode.dialog';

@NgModule({
    declarations: [
        PeopleStep2Component,
        PeopleStep3Component,
        PeopleStep4Component,
        PeopleStep5Component,
        PeopleStep6Component,
        GeocodeDialog
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
        ComponentModule,
        MenuModule,
        HttpClientModule,

        FuseCardModule,

        FuseCardModule,
        NgxFilesizeModule,
        GoogleMapsModule
    ],
    exports: [
        PeopleStep2Component,
        PeopleStep3Component,
        PeopleStep4Component,
        PeopleStep5Component,
        PeopleStep6Component,
    ]
})
export class PeopleModule
{
    public static maxStep: number = 6;
}
