INSERT INTO users (username, password_hash, salt) VALUES ('another', 'lxBzK/ae45XjE2KV2arEkCSGGRE=', 'K0o6Nv1ykMc='),
('ima', 'lxBzK/ae45XjE2KV2arEkCSGGRE=', 'K0o6Nv1ykMc='),('eric', 'lxBzK/ae45XjE2KV2arEkCSGGRE=', 'K0o6Nv1ykMc=');

INSERT INTO accounts (user_id, balance) VALUES ((SELECT user_id FROM users WHERE username = 'another'), 1000),
((SELECT user_id FROM users WHERE username = 'ima'), 1000),((SELECT user_id FROM users WHERE username = 'eric'), 1000);