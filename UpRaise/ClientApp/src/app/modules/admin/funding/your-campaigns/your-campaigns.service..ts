import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'environments/environment';
import { TwoFactorUserSecurityDTO, TwoFactorAuthAuthenticatorDetailDTO, ResultDTO, EnumDTO, StatusEnum } from 'app/models';
import { map, switchMap, tap } from 'rxjs/operators';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { IYourCampaignDataDTO, IYourCampaignRequestDTO, IYourCampaignResponseDTO } from './your-campaigns.types';

@Injectable({ providedIn: 'root' })
export class YourCampaignsService {

    private BASE_URL: string = 'api/campaigns/';

    private _yourCampaigns: BehaviorSubject<IYourCampaignResponseDTO | null> = new BehaviorSubject(null);

    constructor(private _httpClient: HttpClient) { }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Getter for campaigns
     */
    get yourCampaigns$(): Observable<IYourCampaignResponseDTO> {
        return this._yourCampaigns.asObservable();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------


    getYourCampaigns(yourCampaignRequestDTO: IYourCampaignRequestDTO): Observable<IYourCampaignResponseDTO> {
        return this._httpClient.post<IYourCampaignResponseDTO>(`${this.BASE_URL}yourCampaigns`, yourCampaignRequestDTO).pipe(
            tap((resultDTO: any) => {

                if (resultDTO.status === StatusEnum.Success) {
                    if (resultDTO.data) {

                        var yourCampaignResponse: IYourCampaignResponseDTO = resultDTO.data;

                        for (let i = 0; i < yourCampaignResponse.yourCampaigns.length; i++) {

                            if (yourCampaignResponse.yourCampaigns[i].startDate)
                                yourCampaignResponse.yourCampaigns[i].startDate = new Date(yourCampaignResponse.yourCampaigns[i].startDate);

                            if (yourCampaignResponse.yourCampaigns[i].endDate)
                                yourCampaignResponse.yourCampaigns[i].endDate = new Date(yourCampaignResponse.yourCampaigns[i].endDate);

                            if (yourCampaignResponse.yourCampaigns[i].createdAt)
                                yourCampaignResponse.yourCampaigns[i].createdAt = new Date(yourCampaignResponse.yourCampaigns[i].createdAt);

                            if (yourCampaignResponse.yourCampaigns[i].updatedAt)
                                yourCampaignResponse.yourCampaigns[i].updatedAt = new Date(yourCampaignResponse.yourCampaigns[i].updatedAt);
                        }

                        this._yourCampaigns.next(yourCampaignResponse);
                    }
                }
                else {
                    this._yourCampaigns.next(null);
                }

                
            })
        );
    }

}
