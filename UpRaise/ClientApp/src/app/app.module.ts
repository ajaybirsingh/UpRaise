import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ExtraOptions, PreloadAllModules, RouterModule } from '@angular/router';
import { MarkdownModule } from 'ngx-markdown';
import { FuseModule } from '@fuse';
import { FuseConfigModule } from '@fuse/services/config';
import { FuseMockApiModule } from '@fuse/lib/mock-api';
import { CoreModule } from 'app/core/core.module';
import { appConfig } from 'app/core/config/app.config';
import { mockApiServices } from 'app/mock-api';
import { ErrorHandler } from '@angular/core';
import { LayoutModule } from 'app/layout/layout.module';
import { AppComponent } from 'app/app.component';
import { appRoutes } from 'app/app.routing';
import { ApplicationInsightsErrorHandler } from './core/applicationinsights/applicationinsightserrorhandler.service';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
//import { ErrorHandler } from './core/applicationinsights/applicationinsightserrorhandler.service';
import { HttpClientModule, HttpClientJsonpModule } from '@angular/common/http';

const routerConfig: ExtraOptions = {
    scrollPositionRestoration: 'enabled',
    preloadingStrategy       : PreloadAllModules
};

@NgModule({
    declarations: [
        AppComponent
    ],
    imports     : [
        BrowserModule,
        BrowserAnimationsModule,
        RouterModule.forRoot(appRoutes, routerConfig),

        // Fuse, FuseConfig & FuseMockAPI
        FuseModule,
        FuseConfigModule.forRoot(appConfig),
        //FuseMockApiModule.forRoot(mockApiServices),

        // Core module of your application
        CoreModule,

        // Layout module of your application
        LayoutModule,

        HttpClientModule,
        HttpClientJsonpModule,

        // 3rd party modules that require global configuration via forRoot
        MarkdownModule.forRoot({}),

        MatMomentDateModule
    ],
    providers: [{
        provide: ErrorHandler,
        useClass: ApplicationInsightsErrorHandler
    },],
    bootstrap   : [
        AppComponent
    ]
})
export class AppModule
{
}
