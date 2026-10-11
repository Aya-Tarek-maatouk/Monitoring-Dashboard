export type ServerStatus = 'Unknown' | 'Up' | 'Down';
export type AlertStatus = 'Triggered' | 'Acknowledged' | 'Resolved';
export type ReportStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';

export interface Page<T> { items: T[]; page: number; pageSize: number; totalCount: number; }
export interface ServerRecord { serverId: number; name: string; ipAddress: string | null; status: ServerStatus; description: string | null; createdAt: string; }
export interface MetricRecord { metricId: number; serverId: number; cpuUsage: number; memoryUsage: number; diskUsage: number; responseTime: number; status: ServerStatus; timestamp: string; }
export interface AlertRecord { alertId: number; serverId: number; metricType: string; metricValue: number; threshold: number; status: AlertStatus; createdAt: string; resolvedAt: string | null; }
export interface ReportRecord { reportId: number; serverId: number; startTime: string; endTime: string; status: ReportStatus; hasDownload: boolean; createdAt: string; completedAt: string | null; }
export interface BackgroundJobRecord { jobId: string; jobName: string; state: string; stateChangedAt: string | null; error: string | null; canRetry: boolean; }
export interface BackgroundJobCounts { enqueued: number; scheduled: number; processing: number; succeeded: number; failed: number; }
export interface BackgroundJobPage { items: BackgroundJobRecord[]; page: number; pageSize: number; totalCount: number; counts: BackgroundJobCounts; sampledAt: string; }
export interface AuthenticationResponse { userId: number; userName: string; role: string; permissions: string[]; accessToken: string; expiresAt: string; }
export interface UserRecord { userId: number; userName: string; email: string; createdAt: string; roleId: number; role: string; permissions: string[]; }
export interface PermissionRecord { permissionId: number; name: string; description: string | null; }
export interface RoleRecord { roleId: number; name: string; description: string | null; permissions: PermissionRecord[]; }
export interface CreateServerPayload { name: string; ipAddress: string | null; description: string | null; status: ServerStatus; }
