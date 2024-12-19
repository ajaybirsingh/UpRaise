import { Route } from '@angular/router';
import { CampaignComponent } from 'app/modules/admin/dashboards/campaign/campaign.component';
import { CampaignResolver } from 'app/modules/admin/dashboards/campaign/campaign.resolvers';

export const campaignRoutes: Route[] = [
    {
        path     : '',
        component: CampaignComponent,
        resolve  : {
            campaigns: CampaignResolver
        }
    }
];
