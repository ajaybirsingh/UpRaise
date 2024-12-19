import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { EnumDTO } from 'app/models';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NewCampaignService } from './new-campaign.service';
import { IEditCampaign } from './new-campaign.types';


@Injectable({
    providedIn: 'root'
})
export class NewCampaignResolver implements Resolve<any>
{
    /**
     * Constructor
     */
    constructor(
        private _router: Router,
        private _newCampaignService: NewCampaignService
    ) {
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
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<IEditCampaign> {
        const id = route.paramMap.get('id');

        return this._newCampaignService.getEditCampaignById(id)
            .pipe(
                // Error here means the requested task is not available
                catchError((error) => {

                    // Log the error
                    console.error(error);

                    // Get the parent url
                    //const parentUrl = state.url.split('/').slice(0, -2).join('/');

                    // Navigate to there
                    this._router.navigateByUrl('/home');

                    // Throw an error
                    return throwError(error);
                })
            );
            
    }
}
