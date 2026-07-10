-- ToDo 管理画面 結合テスト用テストデータ
-- テストケース: TC-001 タスク追加、TC-005 フィルタ、TC-006 データ永続化で使用

INSERT INTO Tasks (Id, Title, DueDate, Priority, IsCompleted, CreatedAt, UpdatedAt)
VALUES ('11111111111111111111111111111111', '買い物', '2026-07-15T00:00:00', 'Medium', 0, '2026-07-10T00:00:00', '2026-07-10T00:00:00');

INSERT INTO Tasks (Id, Title, DueDate, Priority, IsCompleted, CreatedAt, UpdatedAt)
VALUES ('22222222222222222222222222222222', 'レポート', '2026-07-20T00:00:00', 'High', 1, '2026-07-10T00:00:00', '2026-07-10T00:00:00');

INSERT INTO Tasks (Id, Title, DueDate, Priority, IsCompleted, CreatedAt, UpdatedAt)
VALUES ('33333333333333333333333333333333', 'メール確認', '2026-07-18T00:00:00', 'Low', 0, '2026-07-10T00:00:00', '2026-07-10T00:00:00');
