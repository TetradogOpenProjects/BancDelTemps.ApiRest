<?php

namespace Database\Seeders;

use Illuminate\Database\Seeder;

use App\Models\Permission;

class PermissionSeed extends Seeder
{
    /**
     * Run the database seeds.
     *
     * @return void
     */
    public function run()
    {
        $permissions=[Permission::ADMIN,Permission::MOD,Permission::VALIDATED_USER];
        
        for($i=0,$f=count($permissions);$i<$f;$i++){
            $permission=new Permission();
            $permission->name=$permissions[$i];
            $permission->save();
        }

    }
}
