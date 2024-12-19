import { NgModule } from '@angular/core';
import { Route, RouterModule } from '@angular/router';
import { CampaignRedliningComponent } from './campaign-redlining.component';
import { CampaignRedliningResolver } from './campaign-redlining.resolvers';
import { CommonModule } from '@angular/common';
import { SharedPipesModule } from '../../../core/pipes/SharedPipesModule';

const routes: Route[] = [
    {
        path: ':id',
        //path: '',
        component: CampaignRedliningComponent,
        resolve: {
            campaignRedline: CampaignRedliningResolver
        }
    }
];

@NgModule({
    declarations: [
        CampaignRedliningComponent
    ],
    imports     : [
        RouterModule.forChild(routes),
        CommonModule,
        SharedPipesModule
    ]

})
export class CampaignRedliningModule
{
}
