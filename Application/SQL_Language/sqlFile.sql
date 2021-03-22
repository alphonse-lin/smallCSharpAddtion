select * from construction_cost
select * from cities
select * from building_functions

insert into construction_cost(name,year,city_id,func_id,price_min,price_max)values(@name,@year,@city_id,@func_id,@price_min,@price_max)

TRUNCATE construction_cost;

select bf.name as function_name, cc.name as quality_name,cc.year, cc.price_max, cc.price_min, c.name as city_name, c.lat, c.lon
FROM construction_cost cc, cities c, building_functions bf 
where cc.city_id=c.code and cc.func_id=bf.id; 

update construction_cost  set price_max = replace(price_min , -1.00 , 0)

alter table construction_cost alter column price_min int