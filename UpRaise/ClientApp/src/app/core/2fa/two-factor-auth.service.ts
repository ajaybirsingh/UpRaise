import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { environment } from 'environments/environment';
import { TwoFactorUserSecurityDTO, TwoFactorAuthAuthenticatorDetailDTO, ResultDTO } from 'app/models';
import { StatusEnum, User, ForgotPasswordDTO, ForgetPasswordResetDTO } from '../../models';
import { catchError, switchMap } from 'rxjs/operators';
import { UserService } from '../user/user.service';
import { AuthService } from '../auth/auth.service';

@Injectable({ providedIn: 'root' })
export class TwoFactorAuthService {

    private BASE_URL: string = 'api/2fa/';

    constructor(private _httpClient: HttpClient,
                private _userService: UserService,
                private _authService: AuthService,
    ) { }

    getDetails(): Observable<TwoFactorUserSecurityDTO> {
        return this._httpClient.get<TwoFactorUserSecurityDTO>(`${this.BASE_URL}details`);
    }

    setupAuthenticator(): Observable<TwoFactorAuthAuthenticatorDetailDTO> {
        return this._httpClient.get<TwoFactorAuthAuthenticatorDetailDTO>(`${this.BASE_URL}setupAuthenticator`);
    }

    disable2FA(): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>(`${this.BASE_URL}disable2FA`, {});
    }

    generateRecoveryCodes(): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>(`${this.BASE_URL}generateRecoveryCodes`, {});
    }

    verifyAuthenticator(verification: any): Observable<ResultDTO> {
        return this._httpClient.post<ResultDTO>(`${this.BASE_URL}verifyAuthenticator`, verification);
    }

    loginWith2fa(twoFactorRecoveryCodeLoginRequestDTO: any): Observable<ResultDTO> {

        return this._httpClient.post<ResultDTO>(`${this.BASE_URL}loginwith2fa`, twoFactorRecoveryCodeLoginRequestDTO).pipe(
            switchMap((response: any) => {
                if (response.status === StatusEnum.Success && response.data && response.data.token) {

                    // Store the access token in the local storage
                    this._authService.accessToken = response.data.token;

                    // Set the authenticated flag to true
                    this._authService.mfaAuthenticated(true);

                    // Store the user on the user service
                    this._userService.user = response.data.user;
                }
                else {
                    this._authService.accessToken = '';

                    this._authService.mfaAuthenticated(false);
                    
                    this._userService.user = null;
                }

                // Return a new observable with the response
                return of(response);
            })
        );
    }


}
