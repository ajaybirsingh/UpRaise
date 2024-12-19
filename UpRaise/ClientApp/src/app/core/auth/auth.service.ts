import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthUtils } from 'app/core/auth/auth.utils';
import { UserService } from 'app/core/user/user.service';
import { StatusEnum, User, ForgotPasswordDTO, ForgetPasswordResetDTO } from '../../models';

@Injectable()
export class AuthService {
    private _authenticated: boolean = false;
    /**
     * Constructor
     */
    constructor(
        private _httpClient: HttpClient,
        private _userService: UserService
    ) {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Setter & getter for access token
     */
    set accessToken(token: string) {
        localStorage.setItem('accessToken', token);
    }

    get accessToken(): string {
        return localStorage.getItem('accessToken') ?? '';
    }

    public mfaAuthenticated(authenticated: boolean) {
        this._authenticated = authenticated;
    }


    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Forgot password
     *
     * @param email
     */
    forgotPassword(username: string): Observable<any> {
        var forgotPasswordDTO: { username: string; } = { username };
        return this._httpClient.post('api/auth/forgot-password', forgotPasswordDTO);
    }

    /**
     * Reset password
     *
     * @param password
     */
    resetPassword(forgotPasswordReset: { username: string; resetToken: string, password: string }): Observable<any> {
        return this._httpClient.post('api/auth/reset-password', forgotPasswordReset);
    }

    /**
     * Sign in
     *
     * @param credentials
     */
    signIn(credentials: { username: string; password: string }): Observable<any> {
        // Throw error, if the user is already logged in
        if (this._authenticated) {
            return throwError('User is already logged in.');
        }

        return this._httpClient.post('api/auth/sign-in', credentials).pipe(
            switchMap((response: any) => {
                if (response.status === StatusEnum.Success && response.data && response.data.token) {

                    // Store the access token in the local storage
                    this.accessToken = response.data.token;

                    // Set the authenticated flag to true
                    this._authenticated = true;

                    // Store the user on the user service
                    this._userService.user = response.data.user;
                }
                else {
                    this.accessToken = '';
                    this._authenticated = false;
                    this._userService.user = null;
                }

                // Return a new observable with the response
                return of(response);
            })
        );
    }

    /**
     * Sign in using the access token
     */
    signInUsingToken(): Observable<any> {
        // Renew token
        return this._httpClient.post('api/auth/refresh-access-token', {
            accessToken: this.accessToken
        }).pipe(
            catchError(() =>

                // Return false
                of(false)
            ),
            switchMap((response: any) => {

                // Store the access token in the local storage
                this.accessToken = response.data.token;

                // Set the authenticated flag to true
                this._authenticated = true;

                // Store the user on the user service
                this._userService.user = response.data.user;

                // Return true
                return of(true);
            })
        );
    }

    /**
     * Sign out
     */
    signOut(): Observable<any> {
        // Remove the access token from the local storage
        localStorage.removeItem('accessToken');

        // Set the authenticated flag to false
        this._authenticated = false;

        // Return the observable
        return of(true);
    }

    /**
     * Sign up
     *
     * @param user
     */
    //signUp(user: { name: string; email: string; password: string; company: string }): Observable<any>
    signUp(user: User): Observable<any> {
        return this._httpClient.post('api/auth/sign-up', user);
    }

    /**
     * Unlock session
     *
     * @param credentials
     */
    unlockSession(credentials: { email: string; password: string }): Observable<any> {
        return this._httpClient.post('api/auth/unlock-session', credentials);
    }

    /**
     * Check the authentication status
     */
    check(): Observable<boolean> {
        // Check if the user is logged in
        if (this._authenticated) {
            return of(true);
        }

        // Check the access token availability
        if (!this.accessToken) {
            return of(false);
        }

        // Check the access token expire date
        if (AuthUtils.isTokenExpired(this.accessToken)) {
            return of(false);
        }

        // If the access token exists and it didn't expire, sign in using it
        return this.signInUsingToken();
    }

    /*
    googleSignInExternal(googleTokenId: string): Observable<SimpleError | ICredentials> {
        
        return this.httpClient.get(APISecurityRoutes.authRoutes.googlesigninexternal(), {
            params: new HttpParams().set('googleTokenId', googleTokenId)
        })
            .pipe(
                map((result: ICredentials | SimpleError) => {
                    if (!(result instanceof SimpleError)) {
                        this.credentialsService.setCredentials(result, true);
                    }
                    return result;

                }),
                catchError(() => of(new SimpleError('error_signin')))
            );
    }
    */

}
