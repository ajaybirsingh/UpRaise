import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'environments/environment';
import { TwoFactorUserSecurityDTO, TwoFactorAuthAuthenticatorDetailDTO, ResultDTO, EnumDTO } from 'app/models';
import { map, switchMap, tap } from 'rxjs/operators';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { IPublicCampaignDetailContribution } from '../home.types';
import { IPublicCampaignContributionRequestDTO, IPublicCampaignContributionResponseDTO } from './contributions.types';


@Injectable({ providedIn: 'root' })
export class ContributionService {

    private BASE_URL: string = 'api/public/';

    private _contributions: BehaviorSubject<IPublicCampaignContributionResponseDTO | null> = new BehaviorSubject(null);

    constructor(private _httpClient: HttpClient) { }


    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for contributions
     */
    get contributions$(): Observable<IPublicCampaignContributionResponseDTO> {
        return this._contributions.asObservable();
    }


    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    getPublicCampaignContributions(publicCampaignContributionRequestDTO: IPublicCampaignContributionRequestDTO): Observable<IPublicCampaignContributionResponseDTO> {
        return this._httpClient.post<IPublicCampaignContributionResponseDTO>(`${this.BASE_URL}campaigncontributions`, publicCampaignContributionRequestDTO).pipe(
            tap((publicCampaignContributionResponseDTO: IPublicCampaignContributionResponseDTO) => {

                if (publicCampaignContributionResponseDTO) {
                    for (let i = 0; i < publicCampaignContributionResponseDTO.contributions.length; i++) {

                        if (publicCampaignContributionResponseDTO.contributions[i].date)
                            publicCampaignContributionResponseDTO.contributions[i].date = new Date(publicCampaignContributionResponseDTO.contributions[i].date);
                    }
                }

                this._contributions.next(publicCampaignContributionResponseDTO);
            })
        );
    }

}
