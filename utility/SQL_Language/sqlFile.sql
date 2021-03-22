select * from construction_cost
select * from cities
select * from building_functions
select * from quality_functions
select * from landuse;
select * from building_consumption_value;

insert into construction_cost(name,year,city_id,func_id,price_min,price_max)values(@name,@year,@city_id,@func_id,@price_min,@price_max)

DROP TABLE building_consumption_value;

TRUNCATE building_functions;

select bf.name as function_name,qf.quality_name as quality_name,c.name as city_name, cc.price_max, cc.price_min,cc.year,c.lat, c.lon
FROM construction_cost cc, cities c, building_functions bf, quality_functions qf
where cc.city_id=c.code and cc.func_id=bf.id and cc.quality_id=qf.quality_id;

update construction_cost  set price_max = replace(price_min , -1.00 , 0)

alter table construction_cost alter column price_min int

create table quality_functions(
	quality_id serial  primary key,
	quality_name char(40)
);

select * from quality_functions

INSERT INTO quality_functions(quality_name) VALUES
	('high_quality'),
	('medium'),
	('low'),
	('clubhouse'),
	('external_work'),
	('5_star'),
	('3_star'),
	('landlord'),
	('end_user'),
	('basement'),
	('multi_story');

select distinct on(name) * from construction_cost;

ALTER TABLE construction_cost ADD quality_name_id int;

ALTER TABLE building_functions ADD relative_name char(20);

INSERT INTO building_functions(relative_name) VALUES
	('O'),
	('C'),
	('R'),
	('R'),
	('H'),
	('M'),
	(''),
	('W'),
	('S');

select * from construction_cost

ALTER TABLE construction_cost Drop name;

alter table construction_cost MODIFY name  after id;

select bf.name as function_name,qf.quality_name as quality_name, cc.quality_id, c.name as city_name, cc.price_max, cc.price_min,cc.year,c.lat, c.lon
FROM construction_cost cc, cities c, building_functions bf, quality_functions qf
where cc.city_id=c.code and cc.func_id=bf.id and cc.quality_id=qf.quality_id and c.name='北京';

ALTER DATABASE urbanxlab_db SET search_path=public,postgis;

create table landuse(
	landuse_id serial  primary key,
	landuse_name char(40)
);

ALTER TABLE landuse ADD city_id int;
ALTER TABLE landuse ALTER COLUMN city_id TYPE character(20);
ALTER TABLE landuse ALTER COLUMN landuse_name TYPE character(10);
ALTER TABLE landuse RENAME city_id TO city_code;
alter table landuse add foreign key(city_code) references cities(code)  on update cascade on delete cascade;

INSERT INTO landuse(landuse_name, city_code) VALUES
	('R','440300'),
	('C','440300'),
	('GIC','440300'),
	('W','440300'),
	('M','440300');

INSERT INTO building_functions(name,relative_name) VALUES
('public','GIC');


create table building_consumption_value(
	cv_id serial,
	cv_name char(40),
	building_type int,
	cv_max numeric(10,4),
	cv_min numeric(10,4),
	
	Primary Key(cv_id),
	Constraint fk_city_id
    Foreign Key(building_type)
    References building_functions(id)
);

DELETE FROM building_functions WHERE name is null;
select * from building_functions order by id ASC
UPDATE building_functions SET relative_name = 'O';
update building_functions set relative_name='S' where id=9;


TRUNCATE building_consumption_value;
drop table building_population;

update building_functions set id=10 where name='public';

select * from building_consumption_value;
select * from building_population;
select * from building_functions;

select cv.cv_id, cv.cv_name, bf.name, bf.relative_name, cv.cv_max, cv.cv_min 
from building_consumption_value cv, building_functions bf
where cv.building_type=bf.id;

select bp.bp_id, bp.bp_name, bp.bp_layer_min,bp.bp_layer_max, bp.bp_people, bp.bp_far_min, bp.bp_far_max,bp.bp_density_max,bp.bp_green_min,bp.bp_height_max,bf.name, bf.relative_name
from building_population bp, building_functions bf
where bp.bp_func_id=bf.id;

select bf.name as function_name,qf.quality_name as quality_name, cc.quality_id, c.name as city_name, cc.price_max, cc.price_min,cc.year,c.lat, c.lon
FROM construction_cost cc, cities c, building_functions bf, quality_functions qf
where cc.city_id=c.code and cc.func_id=bf.id and cc.quality_id=qf.quality_id and c.name='北京';

update building_population set bp_name='4' where bp_id=5;

create table tree_co2_index(
	id serial primary key,
	name char(40),
	latin_name char(40),
	DBH_min int,
	DHB_max int,
	VE_index_01 numeric(9,7),
	VE_index_02 numeric(9,7),
	htm numeric(4,3),
	DW_density int,
	equation_source char(60),
	root_index numeric(3,2),
	DWB_C_index numeric(2,1),
	C_CO2_weight_ratio numeric(3,2)
);

select * from tree_co2_index

select * from tree_co2_index where name='毛白杨';

ALTER TABLE tree_co2_index ADD htm numeric(4,3);

ALTER TABLE tree_co2_index RENAME htm TO htm_index;
