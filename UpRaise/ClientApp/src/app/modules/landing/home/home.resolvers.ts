import { Injectable } from '@angular/core';
import { catchError, map } from 'rxjs/operators';
import { HttpClient, HttpBackend } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { EnumDTO } from 'app/models';
import { throwError } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { EnumsService } from '../../../core/services/enums.service';
import { HomeService } from './home.service';
import { IPublicCampaignDetail } from './home.types';
import { ExploreMapService } from './explore/explore-map.service';
import { IPublicClosestLocationDTO } from './explore/explore-map.types';

@Injectable({
    providedIn: 'root'
})
export class LandingHomeCategoriesResolver implements Resolve<any>
{
    /**
     * Constructor
     */
    constructor(private _homeService: HomeService, private _enumsService: EnumsService) {
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
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<EnumDTO[]> {
        return this._homeService.getCategories();
    }
}



@Injectable({
    providedIn: 'root'
})
export class LandingHomeListIsSignedInResolver implements Resolve<boolean>
{
    /**
     * Constructor
     */
    constructor(private _authService: AuthService) {
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
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        return this._authService.check();
    }
}




@Injectable({
    providedIn: 'root'
})
export class LandingHomeCampaignResolver implements Resolve<any>
{
    /**
     * Constructor
     */
    constructor(
        private _router: Router,
        private _homeService: HomeService
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
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<IPublicCampaignDetail> {
        const campaignTypeId = route.paramMap.get('type');
        const campaignId = route.paramMap.get('id');
        
        return this._homeService.getCampaignById(campaignTypeId, campaignId)
            .pipe(
                // Error here means the requested task is not available
                catchError((error) => {

                    // Log the error
                    console.error(error);

                    // Get the parent url
                    const parentUrl = state.url.split('/').slice(0, -2).join('/');

                    // Navigate to there
                    this._router.navigateByUrl(parentUrl);

                    // Throw an error
                    return throwError(error);
                })
            );
            
    }
}



@Injectable({
    providedIn: 'root'
})
export class GoogleMapsResolver implements Resolve<boolean>
{
    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient) {
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
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {

        var apiLoaded = this._httpClient.jsonp('https://maps.googleapis.com/maps/api/js?key=AIzaSyClAtPU7lxKAfMdO99UshK7KuowkGjpDEY', 'callback')
            .pipe(
                map(() => {
                    //console.log('resolver explore.component api loaded init');
                    return true;
                }),
                catchError((error) => {
                    console.error(error);
                    return of(false);
                }));

        return apiLoaded;
    }
}




@Injectable({
    providedIn: 'root'
})
export class ClosestLocationResolver implements Resolve<any>
{
    /**
     * Constructor
     */
    constructor(private _exploreMapService: ExploreMapService) {
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
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<IPublicClosestLocationDTO> {
        return this._exploreMapService.getClosestLocation();
    }
}





@Injectable({
    providedIn: 'root'
})
export class LandingHomeCampaignResolverSEOFriendlyURL implements Resolve<any>
{
    /**
     * Constructor
     */
    constructor(
        private _router: Router,
        private _homeService: HomeService
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
    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<IPublicCampaignDetail> {

        if (state.url && state.url.toLowerCase().startsWith('/campaign-')) {

            var url = state.url.substring(10);

            const trimStart = url.indexOf('/');
            if (trimStart > 0)
                url = url.substring(0, trimStart);

            return this._homeService.getCampaignByFriendlyURL(url)
                .pipe(
                    // Error here means the requested task is not available
                    catchError((error) => {

                        // Log the error
                        console.error(error);

                        // Navigate to there
                        this._router.navigateByUrl('/home');

                        // Throw an error
                        return throwError(error);
                    })
                );
        }
        else
            this._router.navigateByUrl('/home');

    }
}
