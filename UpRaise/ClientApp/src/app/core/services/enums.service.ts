import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { IUser } from 'app/models/user';
import { retry, catchError, switchMap } from 'rxjs/operators';
import { ResultDTO, EnumDTO, StatusEnum } from 'app/models';

@Injectable({
    providedIn: 'root'
})
export class EnumsService {
    /**
     * Constructor
     */

    private _organizationCampaignCategories: EnumDTO[] = null;
    private _countries: EnumDTO[] = null;

    constructor(private _httpClient: HttpClient) {
    }

    public async GetCountries(): Promise<EnumDTO[]> {
        if (this._countries != null)
            return this._countries;

        var e = await this.GetEnumAsync(EnumTypes.Countries);

        this._countries = e;

        return e;
    }

    public async GetOrganizationCampaignCategories(): Promise<EnumDTO[]> {
        if (this._organizationCampaignCategories != null)
            return this._organizationCampaignCategories;

        var e = await this.GetEnumAsync(EnumTypes.OrganizationCampaignCategories);

        this._organizationCampaignCategories = e;

        return e;
    }

    private async GetEnumAsync(enumType: EnumTypes): Promise<EnumDTO[]> {
        const resultDTO = await this._httpClient.post<ResultDTO>(`api/common/getenums`, enumType).toPromise();
        if (resultDTO.status === StatusEnum.Success) {
            return resultDTO.data;
        }

        return null;
    }

}



export enum EnumTypes {
    Countries = 1,
    Currencies = 2,
    OrganizationCampaignCategories = 3,
}
