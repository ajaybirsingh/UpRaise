import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { project as projectData } from 'app/mock-api/dashboards/project/data';
import { ResultDTO } from '../../../../models';
import { CampaignTypes } from '../../../../shared/enums';
import { IContributionRequestDTO, IStripeCreateRequestDTO, ICryptoCreateRequestDTO } from './contribution.types';
import { ContributionTypes } from './contribution.enums';

@Injectable({
    providedIn: 'root'
})
export class ContributionService {

    private BASE_URL: string = 'api/contributions/';

    /**
     * Constructor
     */
    constructor(private _httpClient: HttpClient) {
    }

    public stripecreate(campaignId:number, amount: number, token: any): Observable<ResultDTO> {

        var stripeCreateRequest: IStripeCreateRequestDTO = {
            campaignId: campaignId,
            amount: amount,
            token: token.id
        };

        return this._httpClient.post<ResultDTO>(`${this.BASE_URL}stripecreate`, stripeCreateRequest).pipe(
            map((resultDTO) => {
                // Return the course
                return resultDTO;
            })
        );
    }

    public cryptocreate(campaignType: number, campaignId: number, amount: number, comment: string): Observable<ResultDTO> {

        var cryptoCreateRequest: ICryptoCreateRequestDTO = {
            comment: comment,
            campaignType: campaignType,
            campaignId: campaignId,
            amount: amount,
        };

        return this._httpClient.post<ResultDTO>(`${this.BASE_URL}cryptocreate`, cryptoCreateRequest).pipe(
            map((resultDTO) => {
                // Return the course
                return resultDTO;
            })
        );
    }

    public contribution(contributionType: ContributionTypes, campaignType: number, campaignId: number, amount: number, comment: string): Observable<ResultDTO> {

        var contributionRequest: IContributionRequestDTO = {
            contributionType: contributionType,
            campaignType: campaignType,
            campaignId: campaignId,
            amount: amount,
            comment: comment
        };

        return this._httpClient.post<ResultDTO>(`${this.BASE_URL}contribution`, contributionRequest).pipe(
            map((resultDTO) => {
                // Return the course
                return resultDTO;
            })
        );
    }



}
