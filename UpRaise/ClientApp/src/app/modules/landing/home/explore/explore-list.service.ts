import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'environments/environment';
import { TwoFactorUserSecurityDTO, TwoFactorAuthAuthenticatorDetailDTO, ResultDTO, EnumDTO } from 'app/models';
import { map, switchMap, tap } from 'rxjs/operators';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { IPublicCampaignDetailContribution } from '../home.types';


@Injectable({ providedIn: 'root' })
export class ExploreListService {

    constructor(private _httpClient: HttpClient) { }


}
