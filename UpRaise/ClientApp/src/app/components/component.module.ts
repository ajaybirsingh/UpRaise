import { NgModule } from '@angular/core';
import { MediaplayerComponent } from './mediaplayer/mediaplayer.component';
import { PlaylistComponent } from './playlist/playlist.component';

import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';
import { SharedModule } from 'app/shared/shared.module';
import { HttpClientModule } from '@angular/common/http';
import { FuseCardModule } from '@fuse/components/card';

@NgModule({
    declarations: [
        MediaplayerComponent,
        PlaylistComponent,
    ],
    imports: [

        MatButtonModule,
        MatCheckboxModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatRadioModule,
        MatSelectModule,
        MatStepperModule,
        SharedModule,
        HttpClientModule,
        FuseCardModule

    ],
    exports: [
        MediaplayerComponent,
        PlaylistComponent
    ]
})
export class ComponentModule { }
