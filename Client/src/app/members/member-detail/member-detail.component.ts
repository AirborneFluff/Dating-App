import { ThisReceiver } from '@angular/compiler';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PressenceService } from 'src/app/_services/pressence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild("memberTabs", {static: true}) memberTabs: TabsetComponent;
  activeTab: TabDirective;
  member: Member;
  messages: Message[] = [];
  user: User;

  constructor(private memberService: MembersService,
    private route: ActivatedRoute,
    public messageService: MessageService,
    public pressence: PressenceService,
    private accountService: AccountService, private router: Router) { 
      this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
        this.user = user;
        this.router.routeReuseStrategy.shouldReuseRoute = () => false;
      })
    }

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.member = data.member;
    })
    this.route.queryParams.subscribe(params => {
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    })
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === "Messages" && this.messages.length === 0){
      this.messageService.createHubConnection(this.user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  loadMessages() {
    this.messageService.getMessageThread(this.member.username).subscribe(messages => {
      this.messages = messages;
    })
  }

  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}
