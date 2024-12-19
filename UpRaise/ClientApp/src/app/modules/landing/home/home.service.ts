import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'environments/environment';
import { TwoFactorUserSecurityDTO, TwoFactorAuthAuthenticatorDetailDTO, ResultDTO, EnumDTO } from 'app/models';
import { map, switchMap, tap } from 'rxjs/operators';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { IPublicCampaign, IPublicCampaignDetail } from './home.types';

@Injectable({ providedIn: 'root' })
export class HomeService {

    private BASE_URL: string = 'api/public/';

    private _categories: BehaviorSubject<EnumDTO[] | null> = new BehaviorSubject(null);
    private _campaign: BehaviorSubject<IPublicCampaignDetail | null> = new BehaviorSubject(null);
    private _campaigns: BehaviorSubject<IPublicCampaign[] | null> = new BehaviorSubject(null);

    constructor(private _httpClient: HttpClient) { }


    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    get categories$(): Observable<EnumDTO[]> {
        return this._categories.asObservable();
    }

    /**
     * Getter for campaigns
     */
    get campaigns$(): Observable<IPublicCampaign[]> {
        return this._campaigns.asObservable();
    }

    /**
     * Getter for campaign
     */
    get campaign$(): Observable<IPublicCampaignDetail> {
       return this._campaign.asObservable();
    }


    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    getCategories(): Observable<EnumDTO[]> {
        console.log()
        return this._httpClient.get<EnumDTO[]>(`${this.BASE_URL}categories`).pipe(
            tap((response: any) => {
                this._categories.next(response.data);
            })
        );
    }

    getPublicCampaigns(publicCampaignRequestDTO: any): Observable<IPublicCampaign> {
        return this._httpClient.post<IPublicCampaign>(`${this.BASE_URL}campaigns`, publicCampaignRequestDTO).pipe(
            tap((campaigns: any) => {

                if (campaigns) {
                    for (let i = 0; i < campaigns.length; i++) {

                        if (campaigns[i].createdAt)
                            campaigns[i].createdAt = new Date(campaigns[i].createdAt);

                        if (campaigns[i].updatedAt)
                            campaigns[i].updatedAt = new Date(campaigns[i].updatedAt);
                    }
                }

                this._campaigns.next(campaigns);
            })
        );
    }

    /**
   * Get course by id
   */
    getCampaignById(campaignTypeId: string, campaignId: string): Observable<IPublicCampaignDetail> {
        return this._httpClient.get<IPublicCampaignDetail>(`${this.BASE_URL}campaign`, { params: { campaignTypeId, campaignId} }).pipe(
            map((campaign) => {

                if (campaign.createdAt)
                    campaign.createdAt = new Date(campaign.createdAt);

                if (campaign.updatedAt)
                    campaign.updatedAt = new Date(campaign.updatedAt);

                /*
                if (campaign.contributions) {
                    for (let i = 0; i < campaign.contributions.length; i++) {
                        campaign.contributions[i].date = new Date(campaign.contributions[i].date);
                    }
                }
                */

                // Update the course
                this._campaign.next(campaign);

                // Return the course
                return campaign;
            }),
            switchMap((campaign) => {

                if (!campaign) {
                    return throwError(`Could not find campaign (${campaignTypeId}) with id of ${campaignId}!`);
                }

                return of(campaign);
            })
        );
    }


    
    getCampaignByFriendlyURL(campaignFriendlyUrl: string): Observable<IPublicCampaignDetail> {
        return this._httpClient.get<IPublicCampaignDetail>(`${this.BASE_URL}campaignByFriendlyURL`, { params: { campaignFriendlyUrl } }).pipe(
            map((campaign) => {

                if (campaign.createdAt)
                    campaign.createdAt = new Date(campaign.createdAt);

                if (campaign.updatedAt)
                    campaign.updatedAt = new Date(campaign.updatedAt);

                /*
                if (campaign.contributions) {
                    for (let i = 0; i < campaign.contributions.length; i++) {
                        campaign.contributions[i].date = new Date(campaign.contributions[i].date);
                    }
                }
                */

                // Update the course
                this._campaign.next(campaign);

                // Return the course
                return campaign;
            }),
            switchMap((campaign) => {

                if (!campaign) {
                    return throwError(`Could not find campaign (${campaignFriendlyUrl})!`);
                }

                return of(campaign);
            })
        );
    }

}
