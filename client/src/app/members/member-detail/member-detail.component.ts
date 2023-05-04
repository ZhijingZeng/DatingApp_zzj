import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
//we don't have access to the view until our component has been constructed

  @ViewChild('memberTabs',{static: true}) memberTabs?: TabsetComponent
  //we need to access it but we need to first get member(*ngIf member)
  //using root resolver, get the data before our root is activated

  //ViewChild does not like ngIf, so we need to delete it.
  //since we are getting member before this component is constructed
  //we actually do not need ngIf
  member: Member ={} as Member;
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[]=[];
  activeTab?: TabDirective;
  messages: Message[] = [];
  user?:User;
  constructor(private accountService: AccountService, private route: ActivatedRoute, private messageService: MessageService,
     public presenceService: PresenceService, private router: Router) { 
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user=>{
        if(user)
              {
                this.user = user
              }
      }
    });
    this.router.routeReuseStrategy.shouldReuseRoute = ()=> false;
  }
//we do not need to unsubscribe from activatedRoute
//https://stackoverflow.com/questions/41138081/do-i-have-to-unsubscribe-from-activatedroute-e-g-params-observables
  ngOnInit(): void {
    this.route.data.subscribe({
      next: data=>this.member = data['member']
    })
    this.route.queryParams.subscribe({
      next: params =>{
        params['tab'] && this.selectTab(params['tab']) //if(params['tab']) this.selectTab(params['tab'])
      }
    })
    this.galleryOptions = [
      {
        width: '500px',
        height : '500px',
        imagePercent:100,
        thumbnailsColumns:4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ]
    this.galleryImages = this.getImages();
  }
  getImages(){
    if(!this.member) return [];
    const imageUrls = [];
    for (const photo of this.member.photos){
      imageUrls.push({
        small: photo.url,
        medium: photo.url,
        big: photo.url
      })
    }
    return imageUrls
  }

  ngOnDestroy(): void {
    
    this.messageService.stopHubConnecction();
  }

  selectTab(heading: string){
    
    if(this.memberTabs){
      this.memberTabs.tabs.find(x =>x.heading == heading)!.active = true //! turn off typescript, we donot have type safety here
    }
  }

  loadMessages(){
    if(this.member){
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: messages => this.messages = messages
      })
    }
  }
  onTabActivated(data: TabDirective){
    this.activeTab = data;
    if(this.activeTab.heading === 'Messages' && this.user){
      //this.loadMessages();
      
      this.messageService.createHubConnection(this.user, this.member.userName)
    } else {
      this.messageService.stopHubConnecction();
    }
  }
}
