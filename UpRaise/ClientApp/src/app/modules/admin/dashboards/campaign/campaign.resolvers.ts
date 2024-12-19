import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { CampaignService } from 'app/modules/admin/dashboards/campaign/campaign.service';

@Injectable({
    providedIn: 'root'
})
export class CampaignResolver implements Resolve<any>
{
    /**
     * Constructor
     */
    constructor(private _campaignService: CampaignService)
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Resolver
     *
     * @param route
     * @param state
     */
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<any>
    {
        return this._campaignService.getCampaigns();
    }
}
