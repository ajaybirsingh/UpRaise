import { NgModule } from '@angular/core';
import { Route, RouterModule } from '@angular/router';
import { TermsOfServiceComponent } from 'app/modules/pages/terms-of-service/terms-of-service.component';

const termsOfServiceRoutes: Route[] = [
    {
        path     : '',
        component: TermsOfServiceComponent
    }
];

@NgModule({
    declarations: [
        TermsOfServiceComponent
    ],
    imports     : [
        RouterModule.forChild(termsOfServiceRoutes)
    ]
})
export class TermsOfServiceModule
{
}
