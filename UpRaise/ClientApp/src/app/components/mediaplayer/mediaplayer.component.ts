import { Component, OnInit, ViewChild, Input, ElementRef } from '@angular/core';

@Component({
    selector: 'app-mediaplayer',
    templateUrl: './mediaplayer.component.html',
    styleUrls: ['./mediaplayer.component.scss'],
})
export class MediaplayerComponent implements OnInit {
    constructor() { }
    //@ViewChild('videoplayer') videoplayer: HTMLVideoElement;
    @ViewChild('videoplayer', { static: true }) videoplayer: ElementRef;

    @Input() currentVideo;
    progressBarStyle = {
        flexBasis: '0%',
    };

    public showButtons = false;
    public value = 0;
    public min = 0;
    public max = 100;
    public smallStep = 1;


    iconClass = 'k-i-play';
    mouseDown = false;

    togglePlay() {
        const isPaused = this.videoplayer.nativeElement.paused;
        if (isPaused)
            this.videoplayer.nativeElement.play();
        else
            this.videoplayer.nativeElement.pause();

        setTimeout(() => this.updateButton(), 10);
    }

    updateButton() {
        const icon = this.videoplayer.nativeElement.paused ? 'k-i-play' : 'k-i-pause';
        this.iconClass = icon;
    }

    handleVolumeChange(e) {
        const { target } = e;
        const { value } = target;
        this.videoplayer.nativeElement.volume = value;
    }

    handleProgress() {

        if (!Number.isNaN(this.videoplayer.nativeElement.duration)) {

            const percent = (this.videoplayer.nativeElement.currentTime / this.videoplayer.nativeElement.duration) * 100;
            this.value = percent;
            //console.log(this.value);
        }
    }

    public onChange(value: number): void {
        const percentage = value / 100.0;
        const seekTime = percentage * this.videoplayer.nativeElement.duration;

        //console.log(`percentage ${percentage} , seekTime ${seekTime}`);

        this.videoplayer.nativeElement.currentTime = seekTime;
    }

    seek(e: MouseEvent)
    {
        //console.log(e.offsetX + 'ox');


        /*
        const { srcElement: progress, offsetX } = e;
        
    //const { offsetWidth } = progress;
        const offsetWidth = 200;
        const seekTime = (offsetX / offsetWidth) * this.videoplayer.duration;
        this.videoplayer.currentTime = seekTime;
        */
    }

    ngOnInit() {
        //const { nativeElement } = this.videoplayer;
        //this.video = nativeElement;
    }

    ngOnChanges(changes) {
        if (this.currentVideo) {
            this.value = 0;
            //setTimeout(() => this.updateButton(), 10);
        }
    }
}
