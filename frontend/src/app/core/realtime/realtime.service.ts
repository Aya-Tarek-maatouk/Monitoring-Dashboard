import { Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from '../auth/auth.service';
import { AlertRecord, MetricRecord } from '../models/models';

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  readonly connected = signal(false);
  readonly latestMetric = signal<MetricRecord | null>(null);
  readonly latestAlert = signal<AlertRecord | null>(null);
  readonly presence = signal<{ serverId: number; connectionCount: number } | null>(null);
  private connection: HubConnection | null = null;
  private readonly joinedServerIds = new Set<number>();
  constructor(private readonly auth: AuthService) { }

  async connect(): Promise<void> {
    if (this.connection?.state === 'Connected' || this.connection?.state === 'Connecting') return;
    this.connection = new HubConnectionBuilder()
      .withUrl('/hubs/monitoring', { accessTokenFactory: () => this.auth.session()?.accessToken ?? '' })
      .withAutomaticReconnect().configureLogging(LogLevel.Warning).build();
    this.connection.on('MetricUpdated', (metric: MetricRecord) => this.latestMetric.set(metric));
    this.connection.on('AlertRaised', (alert: AlertRecord) => this.latestAlert.set(alert));
    this.connection.on('AlertResolved', (alert: AlertRecord) => this.latestAlert.set(alert));
    this.connection.on('PresenceUpdated', (presence: { serverId: number; connectionCount: number }) => this.presence.set(presence));
    this.connection.onreconnected(() => {
      this.connected.set(true);
      void this.joinSubscriptions();
    });
    this.connection.onclose(() => this.connected.set(false));
    try {
      await this.connection.start();
      this.connected.set(true);
      await this.joinSubscriptions();
    } catch { this.connected.set(false); }
  }

  async joinServer(serverId: number): Promise<void> {
    this.joinedServerIds.add(serverId);
    if (this.connection?.state === 'Connected') await this.connection.invoke('JoinServer', serverId);
  }
  async leaveServer(serverId: number): Promise<void> {
    this.joinedServerIds.delete(serverId);
    if (this.connection?.state === 'Connected') await this.connection.invoke('LeaveServer', serverId);
  }
  async disconnect(): Promise<void> {
    if (this.connection) await this.connection.stop();
    this.connection = null;
    this.joinedServerIds.clear();
    this.connected.set(false);
  }

  private async joinSubscriptions(): Promise<void> {
    if (!this.connection || this.connection.state !== 'Connected') return;
    for (const serverId of this.joinedServerIds) await this.connection.invoke('JoinServer', serverId);
  }
}