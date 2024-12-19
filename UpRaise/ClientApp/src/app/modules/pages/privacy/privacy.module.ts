import { NgModule } from '@angular/core';
import { Route, RouterModule } from '@angular/router';
import { PrivacyComponent } from 'app/modules/pages/privacy/privacy.component';

const privacyRoutes: Route[] = [
    {
        path     : '',
        component: PrivacyComponent
    }
];

@NgModule({
    declarations: [
        PrivacyComponent
    ],
    imports     : [
        RouterModule.forChild(privacyRoutes)
    ]
})
export class PrivacyModule
{
}
